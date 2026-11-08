import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {createRequire, syncBuiltinESMExports} from 'node:module';
import {pathToFileURL} from 'node:url';
import net from 'node:net';
import tls from 'node:tls';
import http from 'node:http';
import https from 'node:https';
import childProcess from 'node:child_process';

const project=path.resolve(process.argv[2]);
const mode=process.argv[3];
process.chdir(project);
const require=createRequire(path.join(project,'package.json'));
const attempts=[];
const deny=kind=>()=>{attempts.push(kind);throw new Error(`Hermetic check blocked ${kind}`);};
net.Socket.prototype.connect=deny('network socket');
tls.connect=deny('TLS socket');
http.request=deny('HTTP request');
https.request=deny('HTTPS request');
globalThis.fetch=deny('global fetch');
for(const key of ['spawn','spawnSync','exec','execSync','execFile','execFileSync','fork'])childProcess[key]=deny('child process '+key);
syncBuiltinESMExports();
const version=name=>JSON.parse(fs.readFileSync(path.join(project,'node_modules',name,'package.json'),'utf8')).version;
assert.equal(process.versions.node,'24.22.1');
assert.equal(version('handlebars'),'4.7.10');
assert.equal(version('minimist'),'1.2.8');
const h=require('handlebars').create();
assert.equal(h.compile('{{#each values}}{{@index}}={{this}};{{/each}}')({values:['a','<b>']}),'0=a;1=&lt;b&gt;;');
h.registerPartial('row','[{{name}}]');
assert.equal(h.compile('{{#each items}}{{> row}}{{/each}}')({items:[{name:'one'},{name:'two'}]}),'[one][two]');
const ast=h.parse('{{#if ok}}safe{{/if}}');
ast.body[0].program.blockParams={length:'0'};
assert.throws(()=>h.compile(ast)({ok:true}));
assert.equal(h.compile('{{lookup subject "constructor"}}')({subject:Function.prototype},{allowProtoMethodsByDefault:true}),'');
const imports=[];
for(const name of (mode==='template'?['semantic-release','@semantic-release/exec','@semantic-release/git','@semantic-release/github','@semantic-release/npm','semantic-release-gitmoji','semantic-release-monorepo']:['semantic-release','@semantic-release/exec','@semantic-release/github','@semantic-release/npm','semantic-release-gitmoji'])){
 const loaded=await import(pathToFileURL(require.resolve(name)).href);assert.ok(Object.keys(loaded).length>0,name);imports.push(name);
}
const gitmoji=require('semantic-release-gitmoji');
const ReleaseNotes=require(path.join(path.dirname(require.resolve('semantic-release-gitmoji')),'lib/release-notes.js'));
const context={options:{repositoryUrl:'https://github.com/example/public-fixture.git'},commits:[],lastRelease:{version:'0.1.0',gitTag:'v0.1.0',gitHead:'b'.repeat(40)},nextRelease:{version:'0.1.1',gitTag:'v0.1.1',gitHead:'a'.repeat(40)},logger:{log(){},warn(){},error(){},success(){}}};
let commitIndex=0;
for(const [emoji,expected] of [['\u{1f4a5}','major'],['\u2728','minor'],['\u{1f41b}','patch']]){
 // Each CLI invocation normally gets a fresh singleton; isolate independent fixtures in this guarded process.
 ReleaseNotes._instance=undefined;
 const title=emoji+' keep release notes compatible (#12)';const checkContext={...context,commits:[{hash:(++commitIndex).toString(16).repeat(40),subject:title,message:title,body:''}]};
 assert.equal(await gitmoji.analyzeCommits({},checkContext),expected);
 assert.match(await gitmoji.generateNotes({},checkContext),/keep release notes compatible/);
}
assert.equal(version('npm'),'11.21.1');
const semver=require('semver');
const installedSem=version('semantic-release');
const readPeer=(name)=>JSON.parse(fs.readFileSync(path.join(path.dirname(require.resolve(name)),'package.json'),'utf8'));
for(const name of ['@semantic-release/exec','@semantic-release/github','@semantic-release/npm','semantic-release-gitmoji']){
 const pkg=JSON.parse(fs.readFileSync(path.join(project,'node_modules',name,'package.json'),'utf8'));
 if(pkg.peerDependencies?.['semantic-release'])assert.equal(semver.satisfies(installedSem,pkg.peerDependencies['semantic-release']),true,name+' peer contract');
}
const callers=[];
if(mode==='space'){
 assert.equal(installedSem,'25.0.0');assert.equal(version('@semantic-release/npm'),'13.0.0');
 const semRequire=createRequire(require.resolve('semantic-release'));
 callers.push([createRequire(semRequire.resolve('@semantic-release/github')),'7.30.1']);
 const yaml=require('js-yaml');assert.deepEqual(yaml.load('safe: true\nvalues: [1, 2]\n'),{safe:true,values:[1,2]});
 const uri=require('fast-uri');assert.equal(uri.parse('https://example.com/path').host,'example.com');
}else{
 assert.equal(installedSem,'25.0.3');assert.equal(version('semantic-release-monorepo'),'8.0.1');assert.equal(version('typescript'),'4.9.5');
 assert.equal(semver.satisfies(installedSem,JSON.parse(fs.readFileSync(path.join(project,'node_modules/semantic-release-monorepo/package.json'),'utf8')).peerDependencies['semantic-release']),true);
 const semRequire=createRequire(require.resolve('semantic-release'));
 callers.push([createRequire(require.resolve('@actions/http-client')),'6.29.1']);
 callers.push([createRequire(semRequire.resolve('@semantic-release/github')),'7.30.1']);
 assert.deepEqual(require('brace-expansion')('fixture-{a,b}.meta'),['fixture-a.meta','fixture-b.meta']);
 const minimatch=require('minimatch');assert.equal(minimatch('Packages/fixture/package.json','Packages/*/package.json'),true);
}
for(const [callerRequire,expected]of callers){
 const api=callerRequire('undici');assert.equal(callerRequire('undici/package.json').version,expected);
 const agent=new api.MockAgent();agent.disableNetConnect();const pool=agent.get('https://fixture.invalid');
 pool.intercept({path:'/data',method:'GET'}).reply(200,{safe:true},{headers:{'content-type':'application/json'}});
 assert.deepEqual(await(await api.fetch('https://fixture.invalid/data',{dispatcher:agent})).json(),{safe:true});
 pool.intercept({path:'/encoding',method:'GET'}).reply(200,'x',{headers:{'content-encoding':Array(20).fill('gzip').join(',')}});
 await assert.rejects(async()=>{const response=await api.fetch('https://fixture.invalid/encoding',{dispatcher:agent});await response.text();});await agent.close();
}
assert.deepEqual(attempts,[]);console.log(JSON.stringify({project:path.basename(project),mode,node:process.version,semanticRelease:installedSem,imports,networkAttempts:0,childProcessAttempts:0,passed:true}));

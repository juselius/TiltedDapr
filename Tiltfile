name='tilted-dapr'
namespace='acmeco'
host_cluster='k2'

user=os.getenv('USER')
vcluster='vcluster_{user}_{namespace}_{host_cluster}'.format(
        user=user,
        namespace=namespace,
        host_cluster = host_cluster)
allow_k8s_contexts(vcluster)

load('ext://restart_process', 'docker_build_with_restart')
load('ext://namespace', 'namespace_inject')

repository='registry.gitlab.com/acmeco/{name}'.format(name=name)
host='{user}-tilt-{name}.dev.acmeco.io'.format(name=name, user=user)

local("tar fc - review |tar xf - --transform='s/^review/_tilt/'", dir='./helm', quiet=True)
local("sed -ri 's/x-review-/{user}-tilt-/' _tilt/*.yaml".format(user=user), dir='./helm', quiet=True)
local("sed -ri 's/x-review-/{user}-tilt-/' _tilt/*.json".format(user=user), dir='./helm', quiet=True)

clientPort=os.getenv('CLIENT_PORT')
if not clientPort:
    clientPort=8080
    os.putenv('CLIENT_PORT', '{0}'.format(clientPort))
else:
    clientPort=int(clientPort)

serverPort=os.getenv('SERVER_PROXY_PORT')
if not serverPort:
    serverPort=8095
    os.putenv('SERVER_PROXY_PORT', '{0}'.format(serverPort))
else:
    serverPort=int(serverPort)

docker_build_with_restart(
    '{repository}:latest'.format(repository=repository),
    entrypoint='dotnet /app/Server.dll',
    context='.',
    dockerfile='Dockerfile',
    only=['./dist'],
    live_update=[sync('./dist', '/app')]
)

manifest=helm(
    'helm',
    name='{user}-tilt-{name}'.format(name=name, user=user),
    namespace=namespace,
    values=[ './helm/values.yaml', './helm/_tilt/values.yaml' ],
    set=[
        'securityContext.runAsNonRoot=false',
        'securityContext.runAsUser=0',
        'image.repository={repository}'.format(repository=repository),
        'ingress.hosts[0].host={host}'.format(host=host),
        'ingress.tls[0].hosts[0]={host}'.format(host=host),
        'ingress.tls[0].secretName={user}-{name}-tls'.format(name=name, user=user)
    ]
)

local('cat > _manifest.yaml',
      dir='./helm/base',
      stdin=manifest,
      echo_off=False, quiet=True)

k8s_yaml(namespace_inject(kustomize('./helm/_tilt'), namespace))

local_resource(
    'create-bundle',
    cmd='dotnet run bundledebug',
    trigger_mode=TRIGGER_MODE_MANUAL
)

local_resource(
    'build-server',
    cmd='dotnet publish -o ./dist src/Server',
    deps=[
        './src/Server',
        './src/Shared'
    ],
    ignore=[
        'src/Server/bin',
        'src/Server/obj',
        'src/Shared/bin',
        'src/Shared/obj',
    ],
    resource_deps=['create-bundle'],
    auto_init=True,
    labels=['server']
)

local_resource(
    'run-client',
    serve_cmd='dotnet fable watch -o build/client --run vite -c ../../vite.config.js',
    serve_dir='./src/Client',
    links=['https://{name}.local.acmeco.io:{port}'.format(name=name, port=clientPort)],
    resource_deps=['build-server'],
    auto_init=True,
    trigger_mode=TRIGGER_MODE_MANUAL,
    labels=['client']
)

k8s_resource(
    '{user}-tilt-{name}'.format(user=user, name=name),
    port_forwards=['{port}:8085'.format(port=serverPort)],
    labels=['server']
)

# vim:ft=python
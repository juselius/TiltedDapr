with import <nixpkgs> {};
let
    port = 3000;
in
pkgs.mkShell {
  buildInputs = [
      dapr-cli
      kustomize
      nodejs
      # tilt
  ];

  DOCKER_BUILDKIT = 0;

  LOG_LEVEL = 4;
  CLIENT_PORT = port + 80;
  SERVER_PORT = port + 85;
  SERVER_PROXY_PORT = port + 95;
  TILT_PORT = port + 50;

  shellHook = ''
  '';
}
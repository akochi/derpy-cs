{
    inputs = {
        nixpkgs.url = "nixpkgs";
    };

    outputs = { self, nixpkgs }: let
        derpy = { pkgs }: pkgs.buildDotnetModule rec {
            pname = "derpy";
            version = "1.0.9.0-dev";

            src = ./.;

            projectFile = "derpy.sln";
            nugetDeps = ./deps.nix;

            dotnet-sdk = pkgs.dotnetCorePackages.sdk_5_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_5_0;
            
            executables = [ "Derpy" ];
        };
    in rec {
        packages.x86_64-darwin.default = derpy { pkgs = import nixpkgs { system = "x86_64-darwin"; }; };
        packages.x86_64-linux.default = derpy { pkgs = import nixpkgs { system = "x86_64-linux"; }; };

        nixosModules.default = { pkgs, ... }: let
            derpyPkg = derpy { inherit pkgs; };
        in {
            systemd.services.derpy = {
                enable = true;
                wantedBy = [ "multi-user.target" ];
                serviceConfig = {
                    ExecStart = "${derpyPkg}/bin/Derpy";
                    RestartSec = 5;
                    Restart = "always";
                    WorkingDirectory = "/var/run/Derpy";
                    DynamicUser = true;
                };
            };
        };
    };
}

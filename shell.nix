{ pkgs ? import <nixpkgs> {} }:
let dependancies = python-packages: with python-packages; [
  # none
];
in pkgs.mkShell {
  buildInputs = with pkgs; [
    (python3.withPackages dependancies)
    mono
    msbuild
    dotnetPackages.Nuget
  ];
}

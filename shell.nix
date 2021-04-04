{ pkgs ? import <nixpkgs> {} }:
pkgs.mkShell {
	buildInputs = with pkgs; [
		python3
		mono
		msbuild
		dotnetPackages.Nuget
	];
}

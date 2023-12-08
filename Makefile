.PHONY: clean test publish format build

clean:
	rm -rf ./AuraLang.Test/Integration/Examples/build/pkg/*.go

test: build
	cd AuraLang.Test && dotnet test
	
install: build
	./scripts/install.sh

format:
	dotnet format

build:
	dotnet build
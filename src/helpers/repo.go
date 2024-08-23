package helpers

import (
	"alma/models"
	"errors"
	"os"
	"path"
	"path/filepath"
	"strings"

	"github.com/samber/lo"
)

func GetModuleInfo(args []string) (moduleInfo models.ModuleInfo, error error) {
	repoName, moduleName := GetRepositoryAndModuleName(args)

	if moduleName == "" {
		return models.ModuleInfo{}, errors.New("module name is required")
	}

	sourceDirectory, targetDirectory := GetRepositorySourceAndTargetDirectory(repoName)
	if sourceDirectory == "" {
		return models.ModuleInfo{}, errors.New("source directory not exists")
	}

	sourceDirectoryFolderInfo, err := os.Stat(sourceDirectory)
	if err != nil || !sourceDirectoryFolderInfo.IsDir() {
		return models.ModuleInfo{}, errors.New("source directory not exists")
	}

	moduleNameAsPath := strings.ReplaceAll(moduleName, "/", string(filepath.Separator))
	moduleDirectory := filepath.Join(sourceDirectory, moduleNameAsPath)

	return models.ModuleInfo{
		RepositoryName: repoName,
		ModuleName: moduleName,
		SourceDirectory: sourceDirectory,
        TargetDirectory: targetDirectory,
		ModuleDirectory: moduleDirectory}, nil
}

func GetRepositoryAndModuleName(args []string) (string, string) {
	return getRepositoryAndModuleName(args, false)
}

func getRepositoryAndModuleName(args []string, singleParamIsRepository bool) (string, string) {
	var repositoryName, moduleName string = "", ""

	usefulArgs := lo.Filter(args, func(arg string, index int) bool { return !strings.HasPrefix(arg, "-") })
	if len(usefulArgs) == 1 {
		if singleParamIsRepository {
			repositoryName = usefulArgs[0]
		} else {
			moduleName = usefulArgs[0]
		}
	} else if len(usefulArgs) >= 1 {
		repositoryName = usefulArgs[0]
		moduleName = usefulArgs[1]
	}

	return repositoryName, moduleName
}

func GetRepositorySourceAndTargetDirectory(repoName string) (string, string) {
	repoSourceDirectory, _ := os.Getwd()
	repoTargetDirectory, _ := os.Getwd()
	repoTargetDirectory = path.Join(repoTargetDirectory, "..")

	return GetRepositorySourceAndTargetDirectoryWithFallback(repoName, repoSourceDirectory, repoTargetDirectory)
}

func GetRepositorySourceAndTargetDirectoryWithFallback(repoName string, sourceFallback string, targetFallback string) (string, string) {
	//TODO: Use repository configuration
	return sourceFallback, targetFallback
}

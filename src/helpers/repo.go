package helpers

import (
	"os"
	"path"
	"strings"

	"github.com/samber/lo"
)

func GetRepositoryAndModuleName(args []string) (*string, *string) {
	return getRepositoryAndModuleName(args, false)
}

func getRepositoryAndModuleName(args []string, singleParamIsRepository bool) (*string, *string) {
	var repositoryName, moduleName *string = nil, nil

	usefulArgs := lo.Filter(args, func(arg string, index int) bool { return !strings.HasPrefix(arg, "-") })
	if len(usefulArgs) == 1 {
		if singleParamIsRepository {
			repositoryName = &usefulArgs[0]
		} else {
			moduleName = &usefulArgs[0]
		}
	} else if len(usefulArgs) >= 1 {
		repositoryName = &usefulArgs[0]
		moduleName = &usefulArgs[1]
	}

	return repositoryName, moduleName
}

func GetRepositorySourceAndTargetDirectory(repoName *string) (*string, *string) {
	repoSourceDirectory, _ := os.Getwd()
	repoTargetDirectory, _ := os.Getwd()
	repoTargetDirectory = path.Join(repoTargetDirectory, "..")

	return GetRepositorySourceAndTargetDirectoryWithFallback(repoName, &repoSourceDirectory, &repoTargetDirectory)
}

func GetRepositorySourceAndTargetDirectoryWithFallback(repoName *string, sourceFallback *string, targetFallback *string) (*string, *string) {
	//TODO: Use repository configuration
	return sourceFallback, targetFallback
}

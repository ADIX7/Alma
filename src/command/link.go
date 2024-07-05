package command

import (
	"alma/config"
	"alma/helpers"
	"os"
	"path/filepath"
	"strings"

	"github.com/samber/lo"
)

type LinkCommand struct {
}

type itemToLink struct {
	source string
	target string
}

func (LinkCommand) GetName() string {
	return "link"
}

func (LinkCommand) GetHelpText() {
	println(
		`Usage:                
	alma link [module]
	alma link [repository] [module]

Options:
	--help          Show this message
	-d, --dry-run   Show what would be linked without actually linking`)
}

func (LinkCommand) Run(args []string) {
	repoName, moduleName := helpers.GetRepositoryAndModuleName(args)

	if moduleName == nil {
		println("Module name is required")
		return
	}

	dryRun := lo.ContainsBy(args, func(item string) bool { return (item == "-d" || item == "--dry-run") })

	sourceDirectory, targetDirectory := helpers.GetRepositorySourceAndTargetDirectory(repoName)
	if sourceDirectory == nil {
		println("Source directory not exists")
		return
	}

	sourceDirectoryFolderInfo, err := os.Stat(*sourceDirectory)
	if err != nil || !sourceDirectoryFolderInfo.IsDir() {
		println("Source directory not exists", *sourceDirectory)
		return
	}

	moduleNameAsPath := strings.ReplaceAll((*moduleName), "/", string(filepath.Separator))
	moduleDirectory := filepath.Join(*sourceDirectory, moduleNameAsPath)

	moduleDirectoryFolderInfo, err := os.Stat(moduleDirectory)
	if err != nil || !moduleDirectoryFolderInfo.IsDir() {
		println("Module directory not exists", moduleDirectory)
		return
	}

	almaConfigFilePath, err := os.Stat(filepath.Join(moduleDirectory, ".alma-config.json"))
	moduleConfiguration := &config.ModuleConfiguration{}
	if err == nil && !almaConfigFilePath.IsDir() {
		moduleConfiguration = config.LoadModuleConfiguration(filepath.Join(moduleDirectory, ".alma-config.json"))
		targetDirectory1 := helpers.ResolvePath(moduleConfiguration.Target)
		targetDirectory = &targetDirectory1
	}
	itemsToLink := TraverseTree(
		&moduleDirectory,
		targetDirectory,
		&moduleDirectory,
		targetDirectory,
		moduleConfiguration,
	)

	filteredItemsToLink := lo.Filter(itemsToLink, func(item itemToLink, index int) bool {
		for _, exclude := range moduleConfiguration.Exclude {
			if strings.HasPrefix(item.source, exclude) {
				return false
			}
		}
		return true
	})

	if dryRun {
		println("Dry run. No links will be created. The following links would be created:")
	}

	linkItems(filteredItemsToLink, dryRun)

	// Not yet used things

	_ = targetDirectory
	_ = moduleConfiguration

}

func TraverseTree(
	currentDirectory *string,
	currentTargetDirectory *string,
	moduleDirectory *string,
	targetDirectory *string,
	moduleConfiguration *config.ModuleConfiguration) []itemToLink {
	content, err := os.ReadDir(*currentDirectory)
	if err != nil {
		return nil
	}

	itemsToLink := make([]itemToLink, 0, len(content))
	for _, item := range content {
		if item.IsDir() {
			continue
		}

		if currentDirectory == moduleDirectory && item.Name() == ".alma-config.json" {
			continue
		}

		itemConfigTargetPath := moduleConfiguration.Links[item.Name()]

		var targetPath string
		if itemConfigTargetPath != "" {
			targetPath = helpers.ResolvePathWithDefault(moduleConfiguration.Links[item.Name()], *targetDirectory)
		} else {
			targetPath = filepath.Join(*currentTargetDirectory, item.Name())
		}

		itemsToLink = append(itemsToLink, itemToLink{
			source: filepath.Join(*currentDirectory, item.Name()),
			target: targetPath,
		})
	}

	for _, item := range content {
		if !item.IsDir() {
			continue
		}

		relativePath := getRelativePath(filepath.Join(*currentDirectory, item.Name()), *moduleDirectory)

		itemConfigTargetPath := moduleConfiguration.Links[relativePath]

		if itemConfigTargetPath != "" {
			itemsToLink = append(itemsToLink, itemToLink{
				source: filepath.Join(*currentDirectory, item.Name()),
				target: helpers.ResolvePathWithDefault(itemConfigTargetPath, *targetDirectory),
			})
		} else {
			newCurrentDirectory := filepath.Join(*currentDirectory, item.Name())
			newTargetDirectory := filepath.Join(*currentTargetDirectory, item.Name())
			items := TraverseTree(
				&newCurrentDirectory,
				&newTargetDirectory,
				moduleDirectory,
				targetDirectory,
				moduleConfiguration,
			)

			if items != nil {
				itemsToLink = append(itemsToLink, items...)
			}
		}
	}

	return itemsToLink
}

func linkItems(itemsToLink []itemToLink, dryRun bool) {
	for _, item := range itemsToLink {
		_, err := os.Stat(item.target)
		if err == nil {
			println("Target already exists", item.target)
			continue
		}

		if dryRun {
			println("Linking", item.source, item.target)
			continue
		}

		err = os.Symlink(item.source, item.target)
		if err != nil {
			println("Error while linking", item.source, item.target)
		}
	}
}

func getRelativePath(full string, parent string) string {
	return strings.TrimPrefix(full[len(parent):], string(filepath.Separator))
}

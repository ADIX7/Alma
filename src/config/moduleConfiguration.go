package config

import (
	"alma/helpers"
	"cmp"
	"encoding/json"
	"io"
	"os"
	"slices"
)

type ModuleConfiguration struct {
	Target        string            `json:"target"`
	Links         map[string]string `json:"links"`
	Exclude       []string          `json:"exclude"`
	ExcludeReadme bool              `json:"excludeReadme"`
	Install       string            `json:"install"`
	Configure     string            `json:"configure"`
}

func LoadModuleConfiguration(moduleConfigPath string) *ModuleConfiguration {
	jsonFile, err := os.Open(moduleConfigPath)

	if err != nil {
		return nil
	}

	defer jsonFile.Close()

	byteValue, _ := io.ReadAll(jsonFile)
	var moduleConfigurationRoot map[string]*ModuleConfiguration
	err = json.Unmarshal(byteValue, &moduleConfigurationRoot)
	if err != nil {
		return nil
	}

	platformKeys := make([]string, 0, len(moduleConfigurationRoot))

	for key := range moduleConfigurationRoot {
		if key != "default" && helpers.IsOnPlatform(key) {
			platformKeys = append(platformKeys, key)
		}
	}

	slices.SortFunc(platformKeys, func(i, j string) int {
		return cmp.Compare(i, j)
	})

	var moduleConfiguration *ModuleConfiguration
	if moduleConfigurationRoot["default"] != nil {
		moduleConfiguration = moduleConfigurationRoot["default"]
	}

	for _, platformKey := range platformKeys {
		moduleConfiguration.Merge(moduleConfigurationRoot[platformKey])
	}

	return moduleConfiguration
}

func (moduleConfig *ModuleConfiguration) Merge(mergeConfig *ModuleConfiguration) {
	// Merge links
	mergedLinks := make(map[string]string, len(moduleConfig.Links)+len(mergeConfig.Links))
	for key, value := range moduleConfig.Links {
		mergedLinks[key] = value
	}

	for key, value := range mergeConfig.Links {
		mergedLinks[key] = value
	}

	moduleConfig.Links = mergedLinks

	// Merge excludes
	mergedExcludes := make([]string, len(moduleConfig.Exclude)+len(mergeConfig.Exclude))
	excludeIndex := 0
	for _, value := range moduleConfig.Exclude {
		mergedExcludes[excludeIndex] = value
		excludeIndex++
	}

	for _, value := range mergeConfig.Links {
		mergedExcludes[excludeIndex] = value
		excludeIndex++
	}

	moduleConfig.Exclude = mergedExcludes

	// Simple properties
	if mergeConfig.Target != "" {
		moduleConfig.Target = mergeConfig.Target
	}
	if mergeConfig.Install != "" {
		moduleConfig.Install = mergeConfig.Install
	}
	if mergeConfig.Configure != "" {
		moduleConfig.Configure = mergeConfig.Configure
	}

	moduleConfig.ExcludeReadme = moduleConfig.ExcludeReadme || mergeConfig.ExcludeReadme
}

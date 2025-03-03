package helpers

import (
	"os"
	"path/filepath"
	"strings"
)

func ResolvePath(path string) string {
	return ResolvePathWithDefault(path, "")
}
func ResolvePathWithDefault(path string, currentDirectory string) string {
    skipCombiningCurrentDirectory := false
	if strings.Contains(path, "~") {
		home, err := os.UserHomeDir()
		if err == nil {
			path = strings.ReplaceAll(path, "~", home)
            skipCombiningCurrentDirectory = true
		}
	}

    if currentDirectory != "" && !skipCombiningCurrentDirectory {
        path = filepath.Join(currentDirectory, path)
    }

	path = filepath.FromSlash(path)
	return path
}

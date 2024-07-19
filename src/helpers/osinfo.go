package helpers

import (
	"bufio"
	"os"
	"runtime"
	"strings"
)

const OsIdentifierDefault = "default"
const OsIdentifierLinux = "linux"
const OsIdentifierMac = "macos"
const OsIdentifierWindows = "windows"

const osReleasePathLinux = "/etc/os-release"

func GetOsIdentifier() string {
	baseIdentifier := getBaseOsIdentifier()

	return baseIdentifier + "-" + getNormalizedArch()
}

func IsOnPlatform(platform string) bool {
    return strings.Index(GetOsIdentifier(), platform) == 0
}

func getBaseOsIdentifier() string {
	switch runtime.GOOS {
	case "windows":
		return OsIdentifierWindows
	case "darwin":
		return OsIdentifierMac
	case "linux":
		file, err := os.Open(osReleasePathLinux)

		if err != nil {
			return OsIdentifierLinux
		}
		defer file.Close()

		scanner := bufio.NewScanner(file)
		for scanner.Scan() {
			line := strings.ToLower(scanner.Text())
			if strings.HasPrefix(line, "id=") {
				return OsIdentifierLinux + "-" + strings.Trim(line[3:], "\"")
			}
		}
	}

	return runtime.GOOS
}

func getNormalizedArch() string {
	switch runtime.GOARCH {
	case "amd64":
		return "x64"
	case "386":
		return "x86"
	}

	return runtime.GOARCH
}

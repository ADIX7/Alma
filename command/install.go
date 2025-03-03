package command

import (
	"alma/config"
	"alma/helpers"
	"errors"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"syscall"

	"github.com/samber/lo"
)

type InstallCommand struct {
}

type shellNotFoundError struct {
	shell string
}

func (e *shellNotFoundError) Error() string {
	return "Shell not found"
}

func (InstallCommand) GetName() string {
	return "install"
}

func (InstallCommand) GetHelpText() {
	println("Install a package")
}

func (InstallCommand) Run(args []string) {
	moduleInfo, err := helpers.GetModuleInfo(args)

	if err != nil {
		println(err.Error())
		return
	}

	moduleDirectory := moduleInfo.ModuleDirectory

	dryRun := lo.ContainsBy(args, func(item string) bool { return (item == "-d" || item == "--dry-run") })

	almaConfigFilePath, err := os.Stat(filepath.Join(moduleDirectory, ".alma-config.json"))
	moduleConfiguration := &config.ModuleConfiguration{}

	if err != nil || almaConfigFilePath.IsDir() {
		println("Error: .alma-config.json not found")
		return
	}

	moduleConfiguration = config.LoadModuleConfiguration(filepath.Join(moduleDirectory, ".alma-config.json"))
	installCommand := moduleConfiguration.Install

	if dryRun {
		println("Dry run, otherwise would run " + installCommand)
		return
	}
	println("Running command: " + installCommand)

	switch runtime.GOOS {
	case "linux":
		err := runShellCommand("sh", "-c", installCommand)

		var shellNotFoundError *shellNotFoundError
		if errors.As(err, &shellNotFoundError) {
			println(shellNotFoundError.shell + " not found")
		}

	case "windows":
		err := runShellCommand("pwsh", "-c", installCommand)

		var shellNotFoundError *shellNotFoundError
		if errors.As(err, &shellNotFoundError) {
			println(shellNotFoundError.shell + " not found")
		}
	default:
		println("Unsupported OS")
	}
}

func runShellCommand(shellCommand string, args ...string) error {
	shell, pwshErrshellErr := exec.LookPath(shellCommand)
	if pwshErrshellErr != nil {
		return &shellNotFoundError{shell: shellCommand}
	}

	shellArgs := append([]string{shellCommand}, args...)
	env := os.Environ()
	execErr := syscall.Exec(shell, shellArgs, env)
	if execErr != nil {
		return errors.New("Error running shell command")
	}

	return nil
}

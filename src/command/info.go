package command

import "alma/helpers"

type InfoCommand struct {
}

func (InfoCommand) GetName() string {
	return "info"
}

func (InfoCommand) GetHelpText() {
	println(
		`Usage:                
    alma info
    alma info [module]
    alma info [repository] [module]`)
}

func (InfoCommand) Run(args []string) {
	println("Platform is '" + helpers.GetOsIdentifier() + "'")
}

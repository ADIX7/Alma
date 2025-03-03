package main

import (
	"alma/command"
	"os"
)

func main() {
	commands := []command.Command{
		command.LinkCommand{},
		command.InfoCommand{},
		command.InstallCommand{},
	}

	run(commands, os.Args[1:])
}

package main

import (
	"alma/command"
	"os"
)

func main() {
	commands := []command.Command{
		command.LinkCommand{},
		command.InfoCommand{},
	}

	run(commands, os.Args[1:])
}

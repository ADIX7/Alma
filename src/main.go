package main

import (
	"os"
    "alma/command"
)

func main() {
	commands := []command.Command{
		command.LinkCommand{},
	}

	run(commands, os.Args[1:])
}

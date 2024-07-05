package main

import (
	"alma/command"

	"github.com/samber/lo"
)

func run(commands []command.Command, args []string) {
	command, found := lo.Find(commands, func(c command.Command) bool { return c.GetName() == args[0] })

	if !found {
		println("Command not found")
		return
	}

	commandArgs := args[1:]
	if lo.ContainsBy(commandArgs, func(item string) bool { return (item == "-h" || item == "--help") }) {
		command.GetHelpText()
		return
	}
	command.Run(commandArgs)
}

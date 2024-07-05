package command

type Command interface {
    GetHelpText()
	GetName() string
	Run(args []string)
}


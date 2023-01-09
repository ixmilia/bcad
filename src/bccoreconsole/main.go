package main

import (
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
)

func main() {
	ex, err := os.Executable()
	if err != nil {
		panic(err)
	}

	isWindows := runtime.GOOS == "windows"
	thisDir := filepath.Dir(ex)
	args := append([]string{"coreconsole"}, os.Args[1:]...)
	suffix := ""
	if isWindows {
		suffix = ".exe"
	}

	executable := filepath.Join(thisDir, fmt.Sprintf("bcad%s", suffix))
	cmd := exec.Command(executable, args...)
	cmd.Stdout = os.Stdout
	cmd.Stderr = os.Stderr
	cmd.Stdin = os.Stdin

	err = cmd.Run()
	if err != nil {
		panic(err)
	}

	os.Exit(cmd.ProcessState.ExitCode())
}

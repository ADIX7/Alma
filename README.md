 # Alma

Alma (aka Advanced Link Manager Application) is another dotfiles management tool.

## Installation

**Linux**

```
sudo wget https://github.com/ADIX7/Alma/releases/download/latest/alma-linux -O /usr/local/bin/alma
sudo chmod +x /usr/local/bin/alma
```

**Windows**

This PowerShell command will download the alma.exe to the current folder. Move it to a folder that is in PATH.
```
Invoke-WebRequest https://github.com/ADIX7/Alma/releases/download/latest/Alma.exe -OutFile alma.exe
```

**As a Docker tool**

You can run it with Docker/Podman. You should mount every directory (source and target too) to the same path as they are on the host.
For example, if you have your dotfiles cloned to your home folder and you have your repository.json in ~/.config/alma/repository.json, then you can run this command and the links will be correct on the host.
```
docker run --rm -it -v /home/myuser:/home/myuser -e "HOME=/home/myuser" adix7/alma:latest ...
```

If you don't have repository.json, you can set the WORKDIR (or ALMA_WORKDIR) env var to your repository. For example, if you cloned your dotfiles to ~/dotfiles, you can use this command.
```
docker run --rm -it -v /home/myuser:/home/myuser -e "WORKDIR=/home/myuser/dotfiles" adix7/alma:latest ...
```
# OS Daemon Setup

See also:
- [Skyod readme.md](readme.md).
- https://www.digitalocean.com/community/tutorials/how-to-install-apache-kafka-on-ubuntu-20-04

You can follow instructions here to set-up container (e.g. Docker) image or build a Linux machine using manual or automated process (e.g. Ansible).

## Linux

The rest of this writeup assumes the use of Ubuntu 22. 

Install Net 6
- https://ubuntu.com/blog/install-dotnet-on-ubuntu


### Step 1 - Set up dedicated user for Sky system

Sky is a network server system, the first step is to create a dedicated user for the root `skyod` daemon service.
This minimizes damage to your machine in the event that someone compromises the software (which includes business logic).

We will create dedicated user called `sky`.
```bash
# Log in as non-root sudo
$ sudo adduser sky

# Follow the prompts to set a password and create the `sky` user.

# Add the `sky` user to the sudo group with the adduser command. 
# You possibly would need these privileges to install dependencies:
$ sudo adduser sky sudo

# `sky` user is now ready. Log into the account using su:
$ su -l sky
```


### Step 2 - Initial software setup

We need to create a on-disk structure described in [readme.md](readme.md).
```bash
$ pwd
/home/sky

# make directory for skyod
$ mkdir skyod
$ cd skyod
$ mkdir bin logs data
$ cd bin

... copy your skyod package under /home/sky/bin
```

### Step 3 - setup Systemd files to auto-start the node
In this section, we create [systemd unit files](https://www.digitalocean.com/community/tutorials/understanding-systemd-units-and-unit-files) for the `skyod` service. This will help you perform 
common service actions such as starting, stopping, and restarting `skyod` in a manner consistent with 
other Linux services.

```bash
# Create the unit file for `skyod` daemon:
$ sudo nano /etc/systemd/system/skyod.service
```

Enter this `skyod.service` init script:
```ini
[Unit]
Description=Skyod daemon
Requires=network.target remote-fs.target
After=network.target remote-fs.target

[Service]
Type=simple
User=sky
# Query logs using `journalctl`, set a logical name
SyslogIdentifier=skyod
ExecStart=/home/sky/skyod/bin/skyod

# https://unix.stackexchange.com/questions/594350/send-specific-signals-to-systemd-for-service-shutdown
## ExecStop=/bin/kill -s SIGINT -$MAINPID
KillSignal=SIGINT
Restart=on-abnormal
# Amount of time to wait before restarting the service                        
RestartSec=7

Environment=SKY_HOME=/home/sky

#WARNING!!! Review the following variables below to match you specific case:
Environment=SKY_MACHINE_CONFIG_SUFFIX=.local
Environment=SKY_NODE_DISCRIMINATOR=0a01
Environment=SKY_ROLE=allmix

[Install]
WantedBy=multi-user.target
```

Start `skyod` daemon:
```bash
$ sudo systemctl start skyod
# To ensure that the server has started successfully, check the journal logs for the `skyod` unit:
$ sudo systemctl status skyod
...

# Stop skyod
$ sudo systemctl stop skyod

# Reload config
$ sudo systemctl restart skyod

# Only when need to restart systemd
$ sudo systemctl daemon-reload
```

You should now observe the appearance of `sky/skyod`


Enable auto-start on boot:
```bash
$ sudo systemctl enable skyod
```


### Step 4 - Testing

```
$ sudo journalctl -u skyod
```


### Step 5 - Server Hardening

We would want to restrict the `sky` user for now.

```bash
# Remove the sky user from the sudo group:
$ sudo deluser sky sudo

# Lock the sky users password using the passwd command. 
# This makes sure that nobody can directly log into the server using this account
$ sudo passwd sky -l

# Now, only root or sudo can log-in as sky
$ sudo su - sky

# To unlock the `sky` account, shall a need arise in future
$ sudo passwd sky -u
```








# OS Daemon Setup

See also:
- [Skyod readme.md](readme.md).
- https://www.digitalocean.com/community/tutorials/how-to-install-apache-kafka-on-ubuntu-20-04

You can follow instructions here to set-up container (e.g. Docker) image or build a Linux machine using manual or automated process (e.g. Ansible).

## Linux

The rest of this writeup assumes the use of Ubuntu 22.

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
$ su -l kafka
```


### Step 2 - Initial software setup

We need to create a on-disk structure described in [readme.md](readme.md).
```bash
$ pwd
/home/sky

# make directory for skyod
$ mkdir skyod
```

### Step 3 - setup Systemd files to auto-start the node
SKY_HOME

### Step 4 - Testing

### Step 5 - Server Hardening

We would want to restrict the `sky` user for now.

```bash
# Lock the sky users password using the passwd command. 
# This makes sure that nobody can directly log into the server using this account
$ sudo passwd sky -l

# Now, only root or sudo can log-in as sky
$ sudo su - sky

# To unlock the `sky` account, shall a need arise in future
$ sudo passwd sky -u
```








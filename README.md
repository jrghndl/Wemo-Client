# Wemo-Client
A client that scans a network for Wemo devices. Specifically for Wemo Smart Light Switch IoT device. Includes basic functionality such as turning the light switch on or off and checking its status.

### Commands: 
```wemo scan```

Rescans the network for Wemo devices.

```wemo list```

Lists every Wemo device on the network

```wemo [device] [option]```

Device number can be * to apply to every Wemo device or a number associated with the Wemo device that appears on the Wemo device list.
Options can be on, off, or status.

ex: 

wemo 1 on

wemo * status

```clear```

Clears the console.

```exit```

Exits the program.

### Wemo Device:
https://www.belkin.com/us/p/P-F7C030/

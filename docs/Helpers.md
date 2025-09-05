
## Сделать вход по сертификату

```bash
ssh-keygen -t rsa -b 4096
ssh-copy-id  gtr@192.168.0.221
```

## Disk

```bash
df -h | awk '$1=="/dev/mmcblk0p2"{print $3 "/" $2 " " $5}'
```

## CPU/RAM/Temp 

```bash
watch -n 2 'echo "CPU: $(top -bn1 | grep '"'"'Cpu(s)'"'"' | awk '"'"'{print $2}'"'"')% | RAM: $(free -m | awk '"'"'NR==2{printf "%.1f%%", $3*100/$2}'"'"') | Temp: $(vcgencmd measure_temp)"'
```

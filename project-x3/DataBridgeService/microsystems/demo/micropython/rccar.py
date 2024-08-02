# https://docs.micropython.org/en/latest/rp2/quickref.html#pwm-pulse-width-modulation

# adapted from JEM rccar kit
# https://github.com/kitlab-io/micropython/blob/kits/jem2/rc-car/jem/kits/rccar/rccar.py

from machine import Pin, I2C, PWM
import time
# from drivers.pcf8574 import *

# i2c = I2C(0, scl=Pin(18), sda=Pin(19), freq=100000) #100khz
# pcf8574 = PCF8574(i2c, addr=0x20)

# Initialize PWM for motor speed control
# Assuming PWM capable pins are connected to L293D's enable pins
# Set initial PWM frequency (for example, 1000 Hz)
speed=512
pwmA = PWM(Pin(1))  
pwm0 = PWM(Pin(0), freq=2000, duty_u16=32768)
pwmB = PWM(Pin(4),1000)  
# pwmC = PWM(Pin(21),1000)  
# pwmD = PWM(Pin(22),1000)

print("kit started!")

pwm0.duty_u16(speed) 

pwmA.duty_u16(speed)
pwmB.duty(speed)
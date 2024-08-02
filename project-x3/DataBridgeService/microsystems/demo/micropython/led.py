# MICROPYTHON DEMO
from machine import Pin
from utime import sleep

print("Hello, Pi Pico!")

led = Pin(25, Pin.OUT)
while True:
  led.toggle()
  sleep(0.2)


# CIRCUITPYTHON DEMO
# https://learn.adafruit.com/circuitpython-essentials/circuitpython-pwm

# SPDX-FileCopyrightText: 2018 Kattni Rembor for Adafruit Industries
#
# SPDX-License-Identifier: MIT

"""CircuitPython Essentials: PWM with Fixed Frequency example."""
import time
import board
import pwmio

# LED setup for most CircuitPython boards:
led = pwmio.PWMOut(board.LED, frequency=5000, duty_cycle=0)
import time, board, pwmio, microcontroller
led = pwmio.PWMOut( microcontroller.pin.GPIO25, frequency=5000, duty_cycle=0)
# LED setup for QT Py M0:
# led = pwmio.PWMOut(board.SCK, frequency=5000, duty_cycle=0)

while True:
    for i in range(100):
        # PWM LED up and down
        if i < 50:
            led.duty_cycle = int(i * 2 * 65535 / 100)  # Up
        else:
            led.duty_cycle = 65535 - int((i - 50) * 2 * 65535 / 100)  # Down
        time.sleep(0.01)

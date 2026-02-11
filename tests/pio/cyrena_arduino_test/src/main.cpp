#include <Arduino.h>
#include "led.h"

// LED pin definition
const uint8_t LED_PIN = 13;

Led led(LED_PIN);

void setup() {
  // Initialize serial communication at 115200 baud
  Serial.begin(115200);
  while (!Serial) {
    ; // wait for serial port to connect
  }
}

void loop() {
  led.toggle();
  Serial.print("LED is now ");
  Serial.println(led.isOn() ? "ON" : "OFF");
  delay(500);
}

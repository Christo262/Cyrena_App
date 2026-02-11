#include "led.h"

Led::Led(uint8_t pin)
    : m_pin(pin), m_state(false) {
    pinMode(m_pin, OUTPUT);
    digitalWrite(m_pin, LOW);
}

void Led::toggle() {
    m_state = !m_state;
    digitalWrite(m_pin, m_state ? HIGH : LOW);
}

bool Led::isOn() const {
    return m_state;
}

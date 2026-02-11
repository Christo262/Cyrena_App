#pragma once

#include <Arduino.h>

/**
 * @brief Simple LED abstraction.
 *
 * The class stores the pin number and provides a toggle() method
 * that flips the output state.  The state is kept internally so
 * that multiple instances can be used independently.
 */
class Led {
public:
    /**
     * @brief Construct a new Led object.
     * @param pin The Arduino pin number to drive.
     */
    explicit Led(uint8_t pin);

    /**
     * @brief Toggle the LED state.
     */
    void toggle();

    /**
     * @brief Get the current logical state.
     * @return true if the LED is on.
     */
    bool isOn() const;

private:
    uint8_t m_pin;
    bool m_state;
};

#include "logging/internals/logging_internals.h"
#include <stdio.h>
#include <stdarg.h>

static log_level_t s_current_level = LOG_LEVEL_DEBUG;

const char* _log_level_str(log_level_t level)
{
    switch (level) {
        case LOG_LEVEL_DEBUG: return "D";
        case LOG_LEVEL_INFO:  return "I";
        case LOG_LEVEL_WARN:  return "W";
        case LOG_LEVEL_ERROR: return "E";
        default:              return "?";
    }
}

log_level_t _log_get_current_level(void)
{
    return s_current_level;
}

void _log_set_current_level(log_level_t level)
{
    if (level >= LOG_LEVEL_DEBUG && level <= LOG_LEVEL_NONE) {
        s_current_level = level;
    }
}

void _log_write(log_level_t level, const char* tag, const char* fmt, va_list args)
{
    if (level < _log_get_current_level()) {
        return;
    }

    printf("[%s] %s: ", _log_level_str(level), tag);
    vprintf(fmt, args);
    printf("\n");
}

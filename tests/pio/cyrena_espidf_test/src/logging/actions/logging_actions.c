#include "logging/logging.h"
#include "logging/internals/logging_internals.h"
#include <stdarg.h>

void log_init(void)
{
    _log_set_current_level(LOG_LEVEL_DEBUG);
}

void log_set_level(log_level_t level)
{
    _log_set_current_level(level);
}

void log_debug(const char* tag, const char* fmt, ...)
{
    va_list args;
    va_start(args, fmt);
    _log_write(LOG_LEVEL_DEBUG, tag, fmt, args);
    va_end(args);
}

void log_info(const char* tag, const char* fmt, ...)
{
    va_list args;
    va_start(args, fmt);
    _log_write(LOG_LEVEL_INFO, tag, fmt, args);
    va_end(args);
}

void log_warn(const char* tag, const char* fmt, ...)
{
    va_list args;
    va_start(args, fmt);
    _log_write(LOG_LEVEL_WARN, tag, fmt, args);
    va_end(args);
}

void log_error(const char* tag, const char* fmt, ...)
{
    va_list args;
    va_start(args, fmt);
    _log_write(LOG_LEVEL_ERROR, tag, fmt, args);
    va_end(args);
}

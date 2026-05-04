#ifndef LOGGING_H
#define LOGGING_H

#include "logging/definitions/logging_defs.h"
#include "logging/actions/logging_actions.h"

/* Convenience macros */
#define LOG_DEBUG(tag, fmt, ...) log_debug(tag, fmt, ##__VA_ARGS__)
#define LOG_INFO(tag, fmt, ...)  log_info(tag, fmt, ##__VA_ARGS__)
#define LOG_WARN(tag, fmt, ...)  log_warn(tag, fmt, ##__VA_ARGS__)
#define LOG_ERROR(tag, fmt, ...) log_error(tag, fmt, ##__VA_ARGS__)

#endif /* LOGGING_H */

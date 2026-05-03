#ifndef LOGGING_ACTIONS_H
#define LOGGING_ACTIONS_H

#include "logging/definitions/logging_defs.h"
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

void log_init(void);
void log_set_level(log_level_t level);

void log_debug(const char* tag, const char* fmt, ...);
void log_info(const char* tag, const char* fmt, ...);
void log_warn(const char* tag, const char* fmt, ...);
void log_error(const char* tag, const char* fmt, ...);

#ifdef __cplusplus
}
#endif

#endif /* LOGGING_ACTIONS_H */

#ifndef LOGGING_INTERNALS_H
#define LOGGING_INTERNALS_H

#include "logging/definitions/logging_defs.h"
#include <stdarg.h>

#ifdef __cplusplus
extern "C" {
#endif

void _log_write(log_level_t level, const char* tag, const char* fmt, va_list args);
const char* _log_level_str(log_level_t level);
log_level_t _log_get_current_level(void);
void _log_set_current_level(log_level_t level);

#ifdef __cplusplus
}
#endif

#endif /* LOGGING_INTERNALS_H */

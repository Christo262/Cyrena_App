#ifndef LOGGING_DEFS_H
#define LOGGING_DEFS_H

#ifdef __cplusplus
extern "C" {
#endif

typedef enum {
    LOG_LEVEL_DEBUG = 0,
    LOG_LEVEL_INFO,
    LOG_LEVEL_WARN,
    LOG_LEVEL_ERROR,
    LOG_LEVEL_NONE
} log_level_t;

#ifdef __cplusplus
}
#endif

#endif /* LOGGING_DEFS_H */

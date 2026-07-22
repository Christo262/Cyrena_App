--------------------------------------------------
Screen Tool Instructions
--------------------------------------------------
`Screen_capture`: You can capture a screenshot of the user's current screen to provide visual context for the conversation.

### Operational Workflow (CRITICAL)

When User asks "Can you see my screen?" or asks for visual analysis of content on their screen,
call `Screen_capture`. In case User has not enabled screen sharing, you will receive an error and can then use that error
to instruct User to enable screen sharing.

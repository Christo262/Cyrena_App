# PlatformIO Development  

## Requirements  

- Visual Studio Code installed.  
- PlatformIO extension added to VS Code.  
- Cyréna PlatformIO extension (`cyrena.platformio`) installed and enabled.  

## What Cyréna Indexes  

### Core Project Layout (Arduino & ESP‑IDF)  

| Folder / File | Content | Access |
|---------------|---------|--------|
| **src** | All sub‑folders; `.c`, `.cpp`, `.h` source files. | Read / write |
| **include** | All sub‑folders; header (`.h`) files. | Read / write |
| **lib** | All sub‑folders; `.c`, `.cpp`, `.h` library files. | Read‑only |
| **platformio.ini** | Project configuration file. | Read‑only |

### Additional Folders for ESP‑IDF Projects  

| Folder / File | Content | Access |
|----------------|---------|--------|
| **managed_components** | All sub‑folders; `.c`, `.cpp`, `.h` files. | Read‑only |
| **components** | All sub‑folders; `.c`, `.cpp`, `.h` files. | Read‑only |
| **sdkconfig*** | ESP‑IDF configuration files. | Read‑only |

## Getting Started  

1. **Create a PlatformIO project** in Visual Studio Code.  
2. **Open Cyréna** and start a **New Chat**.  
3. Expand the **Embedded** shortcuts.  
4. Click **PlatformIO**.  
5. In the dialog that appears:  
   - Enter a title for the chat.  
   - Provide the full path to the `platformio.ini` file (or browse to select it).  
   - Choose the AI connection you wish to use.  
   - Optionally enable or disable specific Cyréna features.  
6. Press **Submit**.  
7. Begin chatting with the AI to:  
   - Review code.  
   - Add or modify source files.  
   - Resolve build issues.  
   - Ask any other project‑specific questions.  

---  

*All indexed folders and files are accessed according to the permissions shown above. Cyréna will never modify read‑only items.*
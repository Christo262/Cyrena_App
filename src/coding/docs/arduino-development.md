# Arduino IDE Support  

Cyréna can interpret the flat‑file layout used by Arduino IDE sketches and assist throughout the development cycle.  

## Requirements  

- Arduino IDE installed on the workstation.  
- Cyréna Arduino IDE extension (`cyrena.arduino_ide`) installed and enabled.  

## Getting Started  

1. **Create or locate a sketch** – save a new `.ino` file or open an existing one.  
2. **Open Cyréna** and start a **New Chat**.  
3. Expand the **Embedded** shortcuts.  
4. Click **Arduino IDE**.  
5. In the dialog that appears:  
   - Provide the full path to the `.ino` sketch (or browse to select it).  
   - Give the chat a descriptive name.  
   - Choose the AI connection you wish to use.  
   - Enter the target board’s name, RAM size, and clock speed.  
   - Enable or disable any additional features you need.  
6. Press **Submit**.  
7. Begin interacting with the AI, for example:  
   - Request a code review.  
   - Ask for a bug fix.  
   - Request any other assistance related to the sketch.  

---  

*The flat structure of an Arduino sketch (a single `.ino` file plus optional libraries) is fully supported. Cyréna will respect the board specifications you provide to generate correct code and suggestions.*
let audioContext;
let mediaStream;
let source;
let processor;
let recordedChunks = [];

window.audioRecorder = {
    startRecording: async function () {
        mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });

        audioContext = new (window.AudioContext || window.webkitAudioContext)({
            sampleRate: 16000
        });

        source = audioContext.createMediaStreamSource(mediaStream);

        processor = audioContext.createScriptProcessor(4096, 1, 1);
        recordedChunks = [];

        processor.onaudioprocess = (e) => {
            const input = e.inputBuffer.getChannelData(0);
            recordedChunks.push(new Float32Array(input));
        };

        source.connect(processor);
        processor.connect(audioContext.destination);

        console.log("Recording WAV...");
    },

    stopRecording: async function () {
        if (!audioContext) return null;

        processor.disconnect();
        source.disconnect();

        mediaStream.getTracks().forEach(track => track.stop());

        const wavBlob = encodeWAV(recordedChunks, 16000);

        await audioContext.close();

        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.readAsDataURL(wavBlob);
            reader.onloadend = () => resolve(reader.result);
        });
    }
};

function encodeWAV(chunks, sampleRate) {
    const samples = mergeBuffers(chunks);
    const buffer = new ArrayBuffer(44 + samples.length * 2);
    const view = new DataView(buffer);

    writeString(view, 0, "RIFF");
    view.setUint32(4, 36 + samples.length * 2, true);
    writeString(view, 8, "WAVE");
    writeString(view, 12, "fmt ");
    view.setUint32(16, 16, true);
    view.setUint16(20, 1, true); // PCM
    view.setUint16(22, 1, true); // Mono
    view.setUint32(24, sampleRate, true);
    view.setUint32(28, sampleRate * 2, true);
    view.setUint16(32, 2, true);
    view.setUint16(34, 16, true);
    writeString(view, 36, "data");
    view.setUint32(40, samples.length * 2, true);

    floatTo16BitPCM(view, 44, samples);

    return new Blob([view], { type: "audio/wav" });
}

function mergeBuffers(chunks) {
    let length = 0;
    chunks.forEach(chunk => length += chunk.length);

    const result = new Float32Array(length);
    let offset = 0;

    chunks.forEach(chunk => {
        result.set(chunk, offset);
        offset += chunk.length;
    });

    return result;
}

function floatTo16BitPCM(view, offset, input) {
    for (let i = 0; i < input.length; i++, offset += 2) {
        let s = Math.max(-1, Math.min(1, input[i]));
        view.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7FFF, true);
    }
}

function writeString(view, offset, string) {
    for (let i = 0; i < string.length; i++) {
        view.setUint8(offset + i, string.charCodeAt(i));
    }
}


window.tts = {
    initialize: function () {
        window.speechSynthesis.getVoices();
    },

    speak: function (text, r, p, v) {
        return new Promise((resolve, reject) => {
            if (!window.speechSynthesis) {
                reject(new Error("Speech synthesis not supported."));
                return;
            }

            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = "en-US";
            utterance.rate = r;
            utterance.pitch = p;
            utterance.volume = v;

            utterance.onend = () => resolve();
            utterance.onerror = (e) => reject(e.error || "Speech synthesis error");

            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(utterance);
        });
    },

    stop: function () {
        if (window.speechSynthesis) {
            window.speechSynthesis.cancel();
        }
    },

    getVoices: function () {
        return new Promise((resolve) => {
            let voices = speechSynthesis.getVoices();

            if (voices.length > 0) {
                resolve(voices.map(v => ({
                    name: v.name,
                    lang: v.lang,
                    default: v.default
                })));
                return;
            }

            speechSynthesis.onvoiceschanged = () => {
                voices = speechSynthesis.getVoices();
                console.log(voices);
                resolve(voices.map(v => ({
                    name: v.name,
                    lang: v.lang,
                    default: v.default
                })));
            };
        });
    },

    speakWithVoice: function (text, voiceName, r, p, v) {
        return new Promise((resolve, reject) => {
            if (!window.speechSynthesis) {
                reject(new Error("Speech synthesis not supported."));
                return;
            }

            const voices = window.speechSynthesis.getVoices();
            const voice = voices.find(v => v.name === voiceName);

            const utterance = new SpeechSynthesisUtterance(text);
            utterance.voice = voice || null;
            utterance.rate = r;
            utterance.pitch = p;
            utterance.volume = v;

            utterance.onend = () => resolve();
            utterance.onerror = (e) => reject(e.error || "Speech synthesis error");

            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(utterance);
        });
    },

    isSpeaking: function () {
        return window.speechSynthesis.isSpeaking;
    }
};
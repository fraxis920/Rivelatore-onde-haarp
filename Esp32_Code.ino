#include <Arduino.h>
#include <ELECHOUSE_CC1101_SRC_DRV.h>
#include <EEPROM.h>

// Configurazione antenna
const uint8_t ANTENNA_3MHZ = A0;
const uint8_t ANTENNA_10MHZ = A1;
const uint8_t ANTENNA_24MHZ = A2;
const uint16_t ANTENNA_THRESHOLD = 20;

RadioWaveDetector radio(ANTENNA_3MHZ, ANTENNA_10MHZ, ANTENNA_24MHZ);

// Configurazione batteria
const uint8_t BATTERY_PIN = A3;
const float R6 = 100000.0;
const float R5 = 100000.0;
const float DIVIDER_RATIO = (R6 + R5) / R5;
const float V_REF = 3.3;
const float V_MIN = 3.2;
const float V_MAX = 4.2;

// Configurazione EEPROM
const int INDIRIZZO_CONTATORE = 0;
const size_t EEPROM_SIZE = 1024;
uint16_t numeroDatiSalvati = 0;

// Stato hardware
bool antenna3MHzOK = false;
bool antenna10MHzOK = false;
bool antenna24MHzOK = false;
bool batteryOK = false;
bool hardwareOK = false;

// Prototipi
float readBatteryADCVoltage();
float calculateBatteryVoltage(float vADC);
float calculatePercentage(float vBattery);
bool checkBattery();
bool checkAntenna(uint8_t pin);
bool checkHardware();
void printBatteryStatus(float vADC, float vBattery, float percentage);
void printHardwareError();
void SendData(RadioWave wave);

void setup() {
    Serial.begin(9600);
    delay(500);

    // EEPROM
    EEPROM.begin(EEPROM_SIZE);
    numeroDatiSalvati = EEPROM.read(INDIRIZZO_CONTATORE);

    // Configurazione pin
    pinMode(ANTENNA_3MHZ, INPUT);
    pinMode(ANTENNA_10MHZ, INPUT);
    pinMode(ANTENNA_24MHZ, INPUT);
    pinMode(BATTERY_PIN, INPUT);

    // Avvio radio
    radio.begin();

    // Controllo hardware
    hardwareOK = checkHardware();

    if (!hardwareOK) {
        printHardwareError();
        Serial.println("\nERRORE: hardware non valido.\nMisurazione bloccata.");
        return;
    }

    Serial.println("\nHardware OK.\nAvvio misurazione...\n");
}

void loop() {
    if (!hardwareOK) {
        delay(1000);
        return;
    }

    // Controllo batteria
    if (!checkBattery()) {
        Serial.println("ERRORE: batteria non presente o tensione non valida.");
        hardwareOK = false;
        return;
    }

    // Controllo antenne
    antenna3MHzOK = checkAntenna(ANTENNA_3MHZ);
    antenna10MHzOK = checkAntenna(ANTENNA_10MHZ);
    antenna24MHzOK = checkAntenna(ANTENNA_24MHZ);

    if (!antenna3MHzOK || !antenna10MHzOK || !antenna24MHzOK) {
        Serial.println("ERRORE: una o piu' antenne non risultano disponibili.");
        hardwareOK = false;
        return;
    }

    // Lettura radio
    radio.update();
    uint16_t count = radio.getWaveCount();

    // Gestione dati
    if (count > 0) {
        RadioWave wave = radio.getWave(count - 1);

        // Invio seriale
        if (Serial) {
            SendData(wave);
        } 
        // Salvataggio EEPROM
        else 
        {
            const uint16_t MAX_DATI = (EEPROM_SIZE - 1) / sizeof(RadioWave);

            if (numeroDatiSalvati < MAX_DATI) 
            {
                int indirizzo = 1 + (numeroDatiSalvati * sizeof(RadioWave));
                EEPROM.put(indirizzo, wave);
                numeroDatiSalvati++;
                EEPROM.write(INDIRIZZO_CONTATORE, numeroDatiSalvati);
                EEPROM.commit();
            }
        }
    }

    delay(122);
}

// Controllo hardware completo
bool checkHardware() {
    Serial.println("\nControllo hardware...");

    batteryOK = checkBattery();
    Serial.println(batteryOK ? "[OK] Batteria" : "[ERRORE] Batteria");

    antenna3MHzOK = checkAntenna(ANTENNA_3MHZ);
    Serial.println(antenna3MHzOK ? "[OK] Antenna 3 MHz" : "[ERRORE] Antenna 3 MHz");

    antenna10MHzOK = checkAntenna(ANTENNA_10MHZ);
    Serial.println(antenna10MHzOK ? "[OK] Antenna 10 MHz" : "[ERRORE] Antenna 10 MHz");

    antenna24MHzOK = checkAntenna(ANTENNA_24MHZ);
    Serial.println(antenna24MHzOK ? "[OK] Antenna 24 MHz" : "[ERRORE] Antenna 24 MHz");

    return batteryOK && antenna3MHzOK && antenna10MHzOK && antenna24MHzOK;
}

// Lettura segnale antenna
bool checkAntenna(uint8_t pin) {
    uint32_t sum = 0;
    for (int i = 0; i < 20; i++) {
        sum += analogRead(pin);
        delay(2);
    }
    return (sum / 20) > ANTENNA_THRESHOLD;
}

// Controllo stato batteria
bool checkBattery() {
    float vADC = readBatteryADCVoltage();
    float vBattery = calculateBatteryVoltage(vADC);
    float percentage = calculatePercentage(vBattery);

    printBatteryStatus(vADC, vBattery, percentage);

    return (vBattery >= V_MIN && vBattery <= V_MAX);
}

// Lettura ADC batteria
float readBatteryADCVoltage() {
    uint32_t adcSum = 0;
    for (int i = 0; i < 10; i++) {
        adcSum += analogRead(BATTERY_PIN);
        delay(5);
    }
    return ((adcSum / 10.0) * V_REF) / 4095.0;
}

// Calcolo tensione reale
float calculateBatteryVoltage(float vADC) {
    return vADC * DIVIDER_RATIO;
}

// Calcolo percentuale carica
float calculatePercentage(float vBattery) {
    float percentage = ((vBattery - V_MIN) / (V_MAX - V_MIN)) * 100.0;
    return constrain(percentage, 0.0, 100.0);
}

// Stampa stato batteria
void printBatteryStatus(const float vADC, const float vBattery, const float percentage) {
    Serial.print("ADC Battery: ");
    Serial.print(vADC, 2);
    Serial.print(" V | Battery: ");
    Serial.print(vBattery, 2);
    Serial.print(" V | Charge: ");
    Serial.print(percentage, 1);
    Serial.println("%");
}

// Stampa errori hardware
void printHardwareError() {
    Serial.println("\n========== ERROR ==========");
    if (!batteryOK) Serial.println("Batteria assente o tensione errata.");
    if (!antenna3MHzOK) Serial.println("Antenna 3 MHz assente.");
    if (!antenna10MHzOK) Serial.println("Antenna 10 MHz assente.");
    if (!antenna24MHzOK) Serial.println("Antenna 24 MHz assente.");
    Serial.println("===========================");
}

// Invio dati seriali
void SendData(RadioWave wave) {
    Serial.print("Frequency: ");
    Serial.println(wave.frequency);
    Serial.print("Ampiezza: ");
    Serial.println(wave.amplitude);
    Serial.print("Durata: ");
    Serial.println(wave.duration);
    Serial.print("Timestamp: ");
    Serial.println(wave.timestamp);
    Serial.print("Type: ");
    Serial.println(wave.type);
}

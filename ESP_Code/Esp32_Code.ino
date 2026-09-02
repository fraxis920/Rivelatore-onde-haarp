#include <Arduino.h>
#include <ELECHOUSE_CC1101_SRC_DRV.h>
#include <EEPROM.h>

const bool DEBUG_MODE = true;

// CC1101
const uint8_t CC1101_315_CSN = 5;
const uint8_t CC1101_433_CSN = 16;
const uint8_t CC1101_868_CSN = 17;

const int16_t CC1101_RSSI_THRESHOLD = -90;

RadioWaveDetector radio(
    CC1101_315_CSN,
    CC1101_433_CSN,
    CC1101_868_CSN,
    CC1101_RSSI_THRESHOLD
);

// Batteria
const uint8_t BATTERY_PIN = A3;
const float R6 = 100000.0;
const float R5 = 100000.0;
const float DIVIDER_RATIO = (R6 + R5) / R5;
const float V_REF = 3.3;
const float V_MIN = 3.2;
const float V_MAX = 4.2;

// EEPROM
const int INDIRIZZO_CONTATORE = 0;
const size_t EEPROM_SIZE = 1024;
uint16_t numeroDatiSalvati = 0;

// Stato hardware
bool batteryOK = false;
bool hardwareOK = false;

// Prototipi
float readBatteryADCVoltage();
float calculateBatteryVoltage(float vADC);
float calculatePercentage(float vBattery);
bool checkBattery();
bool checkHardware();
void printBatteryStatus(float vADC, float vBattery, float percentage);
void printHardwareError();
void SendData(RadioWave wave);

void setup()
{
    Serial.begin(9600);
    delay(2500);

    EEPROM.begin(EEPROM_SIZE);
    numeroDatiSalvati = EEPROM.read(INDIRIZZO_CONTATORE);

    pinMode(BATTERY_PIN, INPUT);

    radio.begin();

    if (DEBUG_MODE)
    {
        hardwareOK = true;
        return;
    }

    hardwareOK = checkHardware();

    if (!hardwareOK)
    {
        printHardwareError();
        Serial.println();
        Serial.println("ERROR: hardware non valido.");
        return;
    }

    Serial.println();
}

void loop()
{
    if (!hardwareOK)
    {
        delay(1000);
        return;
    }

    if (!DEBUG_MODE)
    {
        if (!checkBattery())
        {
            Serial.println(
                "ERROR: batteria non presente o tensione non valida."
            );

            hardwareOK = false;
            return;
        }
    }

    radio.update();

    uint16_t count = radio.getWaveCount();

    if (count > 0)
    {
        RadioWave wave = radio.getWave(count - 1);

        SendData(wave);

        const uint16_t MAX_DATI =
            (EEPROM_SIZE - 1) / sizeof(RadioWave);

        if (numeroDatiSalvati < MAX_DATI)
        {
            int indirizzo =
                1 + (numeroDatiSalvati * sizeof(RadioWave));

            EEPROM.put(indirizzo, wave);

            numeroDatiSalvati++;

            EEPROM.write(
                INDIRIZZO_CONTATORE,
                numeroDatiSalvati
            );

            EEPROM.commit();
        }

        radio.clear();
    }

    delay(1000);
}

bool checkHardware()
{
    Serial.println();

    batteryOK = checkBattery();

    Serial.println(
        batteryOK
        ? "[] Batteria."
        : "[ERROR] Batteria."
    );

    return batteryOK;
}

bool checkBattery()
{
    float vADC = readBatteryADCVoltage();
    float vBattery = calculateBatteryVoltage(vADC);
    float percentage = calculatePercentage(vBattery);

    printBatteryStatus(
        vADC,
        vBattery,
        percentage
    );

    return (
        vBattery >= V_MIN &&
        vBattery <= V_MAX
    );
}

float readBatteryADCVoltage()
{
    uint32_t adcSum = 0;

    for (int i = 0; i < 10; i++)
    {
        adcSum += analogRead(BATTERY_PIN);
        delay(5);
    }

    return (
        (adcSum / 10.0) * V_REF
    ) / 4095.0;
}

float calculateBatteryVoltage(float vADC)
{
    return vADC * DIVIDER_RATIO;
}

float calculatePercentage(float vBattery)
{
    float percentage =
        ((vBattery - V_MIN) /
        (V_MAX - V_MIN)) * 100.0;

    return constrain(
        percentage,
        0.0,
        100.0
    );
}

void printBatteryStatus(
    float vADC,
    float vBattery,
    float percentage
)
{
    Serial.print("ADC Battery: ");
    Serial.print(vADC, 2);

    Serial.print(" V | Battery: ");
    Serial.print(vBattery, 2);

    Serial.print(" V | Charge: ");
    Serial.print(percentage, 1);

    Serial.println("%");
}

void printHardwareError()
{
    Serial.println();
    Serial.println("ERROR:");

    if (!batteryOK)
        Serial.println(
            "Batteria assente o tensione errata."
        );
}

void SendData(RadioWave wave)
{
    Serial.print("Frequenza:");
    Serial.println(wave.frequency);

    Serial.print("Ampiezza:");
    Serial.println(wave.amplitude);

    Serial.print("Durata:");
    Serial.println(wave.duration);

    Serial.print("Timestamp:");
    Serial.println(wave.timestamp);

    Serial.print("Type:");
    Serial.println(wave.type);
}
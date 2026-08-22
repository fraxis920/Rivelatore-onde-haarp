#include <Arduino.h>

// Numero di misurazioni simulate
uint32_t timestamp = 0;

// Genera un float casuale
float randomFloat(float minValue, float maxValue)
{
    return minValue +
           (random(0, 1000) / 1000.0) *
           (maxValue - minValue);
}

// Stampa stato batteria
void printBatteryStatus(
    float vADC,
    float vBattery,
    float percentage)
{
    Serial.print("ADC Battery: ");
    Serial.print(vADC, 2);

    Serial.print(" V | Battery: ");
    Serial.print(vBattery, 2);

    Serial.print(" V | Charge: ");
    Serial.print(percentage, 1);

    Serial.println("%");
}

// Genera e stampa dati batteria
void generateBatteryData()
{
    // Batteria Li-Ion simulata: 3.30 - 4.20 V
    float vBattery = randomFloat(3.30, 4.20);

    // Partitore 100k / 100k
    float vADC = vBattery / 2.0;

    // Conversione tensione -> percentuale
    float percentage =
        ((vBattery - 3.20) /
        (4.20 - 3.20)) * 100.0;

    percentage = constrain(
        percentage,
        0.0,
        100.0
    );

    printBatteryStatus(
        vADC,
        vBattery,
        percentage
    );
}

// Genera e stampa un'onda radio
void generateRadioData()
{
    // Frequenze plausibili
    uint16_t frequency =
        random(70, 101);

    // Ampiezza ADC simulata
    uint32_t amplitude =
        random(30000, 50001);

    // Durata simulata
    uint32_t duration =
        random(500000000, 3000000000UL);

    // Timestamp incrementale
    timestamp += random(500, 5000);

    // Tipo di onda
    uint8_t type =
        random(0, 4);

    Serial.print("Frequenza:");
    Serial.println(frequency);

    Serial.print("Ampiezza:");
    Serial.println(amplitude);

    Serial.print("Durata:");
    Serial.println(duration);

    Serial.print("Timestamp:");
    Serial.println(timestamp);

    Serial.print("Type:");
    Serial.println(type);
}

// Simula il controllo hardware
void generateHardwareStatus()
{
    Serial.println();
    Serial.println("Controllo hardware...");

    Serial.println("[OK] Batteria");
    Serial.println("[OK] Antenna 3 MHz");
    Serial.println("[OK] Antenna 10 MHz");
    Serial.println("[OK] Antenna 24 MHz");

    Serial.println();
    Serial.println("Hardware OK.");
    Serial.println("Avvio misurazione...");
    Serial.println();
}

void setup()
{
    Serial.begin(9600);

    // Inizializzazione generatore casuale
    randomSeed(analogRead(A0));

    delay(2500);


}

void loop()
{
    
generateRadioData();
    Serial.println();

    delay(1000);
}
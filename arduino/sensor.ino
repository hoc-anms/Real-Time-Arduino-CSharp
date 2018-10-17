#include <LiquidCrystal.h>
LiquidCrystal lcd(A0 ,A1 ,A2 ,A3 ,A4 ,A5 );
int32_t Htime;                    
int32_t Ltime;

float Ttime;
float frequency;
float capacitance;


int state = 0;  // Tạo biến sự kiện để điều khiển Arduino
                // Tạo 2 biến để xác định thời gian thực trên Arduino
long time_now = 0; 
long time_start = 0;
//float random_number();  // Tạo Random, giả lập dữ liệu cảm biến và đếm thời gian thực


void setup() {
  pinMode(9,INPUT);            //pin 8 as signal input
    lcd.begin(16, 2);
    lcd.setCursor(0,0);
    lcd.print("capacitance =");
  Serial.begin(9600);   // Khởi tạo giao thức Serial, mình chọn baudrate là 9600

}

void loop() {
  random_number();
  // Điều khiển Arduino qua giá trị của biến state
    if(Serial.available()) 
    {
        char temp = Serial.read();
        if(temp == '0')
            state = 0;
        else if(temp == '1')
            state = 1;
        else if(temp == '2')
            state = 2;
        
    }
 
    // Thực thi các trường hợp với các giá trị của biến state
    switch(state)
    {
        // state = 0: dừng Arduino
        case 0:
        break;
        // state = 1: thực thi hàm tạo Random, xuất dữ liệu và thời gian thực qua Serial, phân tách nhau bởi ký tự gạch đứng “|”
        case 1:
            random_number();
            Serial.print(time_now);
            Serial.print("|");
            Serial.println(capacitance);
        break;
        // state = 2: Reset dữ liệu và thời gian về 0
        case 2:
            capacitance = 0;
            time_now = 0;
            state = 0;
        break;
    }
}
float random_number()
{
    time_start = millis();
    for (int i=0;i<5;i++)        //measure time duration five times
    {
        Ltime=(pulseIn(9,HIGH)+Ltime)/2;        //get average for each cycle
        Htime=(pulseIn(9,LOW)+Htime)/2;
    }
    Ttime = Htime+Ltime;
    frequency=1000000/Ttime;
    
    capacitance = (1.44*1000000000)/(20800*frequency);   //calculating the Capacitance in nF
//    if(capacitance<=200000&&capacitance>=100000)//Hieu chinh...can chinh
//    {
//      capacitance=capacitance*1,056667302;
//    }
//    if(capacitance<=400&&capacitance>=100)
//    {
//      capacitance=capacitance*1,142312128;
//    }
//     if(capacitance<=20000&&capacitance>=10000)
//    {
//      capacitance=capacitance*1,145565131;
//    }
//    lcd.setCursor(0,1);
//    lcd.print(capacitance);
//    lcd.print(" nF   ");
    delay(100);
    time_now = time_now + millis() - time_start;
}

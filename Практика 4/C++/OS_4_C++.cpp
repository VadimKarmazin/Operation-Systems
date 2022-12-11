#include <iostream>
#include <ctime>

using namespace std;

int main() {

    srand(time(0));
    unsigned int a = 0, b = 2, c = 3;

    for (int i = 0; i < 100000000; i++) {
        a += 2 * b + c - i;
    }
    cout << a;
    cout << "\nruntime = " << clock() / 1000.0 << endl; // время работы программы 
    return 0;
}

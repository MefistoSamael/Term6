#!/bin/bash

# Y must be [0;48] in full screen
random_number_generator_y(){
    local current_time=$(date "+%H%M%S%3N")
    local a=226954
    local c=1
    local m=49
    
    echo $(( ($a*$current_time + c) % m ))
}

# X must be [39; 364] in full screen
random_number_generator_x(){
    local current_time=$(date "+%H%M%S%3N")
    local a=226954
    local c=1
    local m=326
    
    echo $(( (($a*$current_time + c) % m) + 39 ))
}

print_time(){
    local counter=0
    local x=$(random_number_generator_x)
    local y=$(random_number_generator_y)
    
    while true
    do
        clear
        print_current_time_with_figlet $x $y
        ((counter++))               
        if [ $counter -eq 10 ]
        then
           x=$(random_number_generator_x)
           y=$(random_number_generator_y)
           counter=0
        fi
        sleep 1s
    done
}

print_current_time_with_figlet(){
    local x=$1
    local y=$2
    local current_time=$(date +%T)

    tput cup $y
    figlet -w $x -c $current_time
}


clear
tput civis
print_time



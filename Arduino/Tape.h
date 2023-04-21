#pragma once
#ifndef __TAPE_H__
#define __TAPE_H__

namespace Tape
{
    /**
     * @brief Loads data from the serial port to the pocket computer
     */
    void Load();

    /**
     * @brief Saves data from pocket computer
     * @param debug Enable debug printing
     */
    void Save(bool debug = false);
}


#endif
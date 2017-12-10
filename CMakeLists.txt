cmake_minimum_required(VERSION 3.8)

project(gomoku CSharp)

set(Targetname pbrain-nexusl)

add_executable(${Targetname} ./gomoku/gomoku/Brain.cs ./gomoku/gomoku/PisqPipe.cs ./gomoku/gomoku/Point.cs)

add_custom_command(TARGET ${Targetname}
                   POST_BUILD
                   COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:${Targetname}> ${CMAKE_CURRENT_BINARY_DIR})

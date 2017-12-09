cmake_minimum_required(VERSION 3.8)

project(gomoku CSharp)

add_executable(pbrain-nexusl ./gomoku/gomoku/Example.cs ./gomoku/gomoku/PisqPipe.cs)

install(TARGETS pbrain-nexusl DESTINATION ./)

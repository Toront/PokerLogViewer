cmake_minimum_required(VERSION 3.27)

project(PokerLogViewer)

set(PROJECT_ROOT ${CMAKE_CURRENT_SOURCE_DIR})
set(SRC_DIR ${PROJECT_ROOT}/src)
set(TESTS_DIR ${PROJECT_ROOT}/tests)
set(SAMPLE_DATA_DIR ${PROJECT_ROOT}/sample-data)

message(STATUS "PokerLogViewer root: ${PROJECT_ROOT}")
message(STATUS "Source dir: ${SRC_DIR}")
message(STATUS "Tests dir: ${TESTS_DIR}")
message(STATUS "Sample data dir: ${SAMPLE_DATA_DIR}")
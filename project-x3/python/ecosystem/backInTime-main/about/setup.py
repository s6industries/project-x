from setuptools import setup, Extension
from Cython.Build import cythonize

ext_modules = [
    Extension('main_cython', ['python_cython/main.pyx']),
    Extension('character_cython', ['src_cy/character.pyx']),
    Extension('camera_cython', ['src_cy/camera.pyx']),
    Extension('create_cython', ['src_cy/create.pyx']),
    Extension('loading_cython', ['src_cy/loading.pyx']),
    Extension('music_cython', ['src_cy/music.pyx']),
    Extension('object_cython', ['src_cy/object.pyx']),
    Extension('reset_cython', ['src_cy/reset.pyx']),
    Extension('ui_cython', ['src_cy/ui.pyx']),
    Extension('timer_cython', ['src_cy/timer.pyx']),

    Extension('base_cython', ['Maps/map_cy/baseMap.pyx']),
    Extension('map2_cython', ['Maps/map_cy/Map_2.pyx']),
    Extension('map3_cython', ['Maps/map_cy/Map_3.pyx']),
    Extension('map4_cython', ['Maps/map_cy/Map_4.pyx']),
]

setup(
    name = 'optimized_game',
    ext_modules=cythonize(ext_modules)
)
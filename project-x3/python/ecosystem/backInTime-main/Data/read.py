import json
import ast

class Read:

    def __init__(self, file):
        self.file = file
        self.data = {}

    def read(self):
        with open(self.file) as file:
            self.data = json.load(file)
            for mapname in self.data:
                self.strToTuple(mapname)

    # convert tupled string in json data to tuple
    def strToTuple(self, namedata):
        for data in self.data[namedata]:
            data['rect'] = ast.literal_eval(data['rect'])
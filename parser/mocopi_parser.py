import struct
import argparse
from datetime import datetime
from collections import namedtuple

StructConf = namedtuple('StructConf', ['nested_name_list', 'data_format_map', 'data_format_with_length_map', 'datetime_name_list'])

class RemainingFieldBytes:
    
    def __init__(self, size):
        self.original_size = size
        self.remaining_size = size

    def consume(self, parsed_length):
        self.remaining_size -= parsed_length
        return self.remaining_size


def get_skdf_struct_conf():
    nested_name_list = [ b'head', b'sndf', b'skdf', b'bons', b'bndt']
    data_format_map = {
            b'vrsn': 'B',
            b'ipad': 'Q',
            b'rcvp': 'H',
            b'bnid': 'H',
            b'pbid': 'H',
            b'tran': '<7f'
        }
    data_format_with_length_map = { b'ftyp': '{}s' }
    datetime_name_list = []
    
    return StructConf(nested_name_list, data_format_map, data_format_with_length_map, datetime_name_list)


def get_fram_struct_conf():
    nested_name_list = [ b'head', b'sndf', b'fram', b'btrs', b'btdt']
    data_format_map = {
            b'vrsn': 'B',
            b'ipad': 'Q',
            b'rcvp': 'H',
            b'fnum': 'I',
            b'time': 'f',
            b'bnid': 'H',
            b'tran': '<7f'
        }
    data_format_with_length_map = { b'ftyp': '{}s' }
    datetime_name_list = [ b'uttm' ]
    
    return StructConf(nested_name_list, data_format_map, data_format_with_length_map, datetime_name_list)


def parse_field(data_format, start_pos, end_pos, binary_data):
    return (end_pos, struct.unpack(data_format, binary_data[start_pos:end_pos]))

def parse_datatime_field(start_pos, end_pos, binary_data):
    data_format = '<d'
    timestamp = struct.unpack(data_format, binary_data[start_pos:end_pos])[0]
    return (end_pos, datetime.utcfromtimestamp(timestamp))

def read_binary_file(file_path, struct_conf):
    nested_name_list = struct_conf.nested_name_list
    data_format_map = struct_conf.data_format_map
    data_format_with_length_map = struct_conf.data_format_with_length_map
    datetime_name_list = struct_conf.datetime_name_list

    with open(file_path, "rb") as file:
        binary_data = file.read()

        binary_length = len(binary_data)
        current_pos = 0
        parent_remaining_bytes_list  = []

        while(current_pos < binary_length) :
            data_format = '<I4s'
            end_pos = current_pos + 8

            (current_pos, unpacked_data) = parse_field(data_format, current_pos, end_pos, binary_data)
            name = unpacked_data[1]
            length = unpacked_data[0]
            parent_remaining_bytes_list.append(RemainingFieldBytes(length))

            if name in nested_name_list:                
                print("  " * (len(parent_remaining_bytes_list) - 1), f'Name: {name}, Length: {length}')
                continue
            
            end_pos = current_pos + length

            if name in data_format_map:
                value_data_format = data_format_map[name]
                (current_pos, unpacked_data) = parse_field(value_data_format, current_pos, end_pos, binary_data)
                print("  " * (len(parent_remaining_bytes_list) - 1), f'Name: {name}, Length: {length}, Value: {unpacked_data}')
            elif name in data_format_with_length_map:
                value_data_format = data_format_with_length_map[name].format(length)
                (current_pos, unpacked_data) = parse_field(value_data_format, current_pos, end_pos, binary_data)
                print("  " * (len(parent_remaining_bytes_list) - 1), f'Name: {name}, Length: {length}, Value: {unpacked_data}')
            elif name in datetime_name_list:
                (current_pos, unpacked_data) = parse_datatime_field(current_pos, end_pos, binary_data)
                print("  " * (len(parent_remaining_bytes_list) - 1), f'Name: {name}, Length: {length}, Value: {unpacked_data}')
            else:
                print(f'Unknown name: {name}')

            while(len(parent_remaining_bytes_list) > 0):
                current_parent_bytes = parent_remaining_bytes_list[-1]
                remaining_size = current_parent_bytes.consume(length)

                if(remaining_size > 0):
                    break
                else:
                    parent_remaining_bytes_list.pop()
                    length = current_parent_bytes.original_size + 8

            current_pos = end_pos


if __name__ == "__main__":
    parser = argparse.ArgumentParser()

    parser.add_argument('input_file', type=str, default="received_messages.bin1")
    parser.add_argument('--type', type=str, default="skdf")
    
    args = parser.parse_args()

    if args.type == 'skdf':
        struct_conf = get_skdf_struct_conf()
        read_binary_file(args.input_file, struct_conf)
    elif args.type == 'fram':
        struct_conf = get_fram_struct_conf()
        read_binary_file(args.input_file, struct_conf)

        
